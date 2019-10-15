---
title: Cdiscount平台对接采坑心得
categories: 跨境电商API
tags:
  - Cdiscount
  - 电商
  - API
date: 2019-10-15 15:46:44
---
本人从事IT行业7年，其中有三年从事跨境电商业务，期间对接了不少的跨境电商平台，颇有心得。但最近对接Cdiscount这个平台的时候，在坑里挣扎了许久，挫败感油然而生。

Cdiscount是目前法国最大的电子商务平台，从它的[Marketplace API](https://dev.cdiscount.com/marketplace/)来看，API这快也算是挺成熟的。文档挺详细的，调用参数、示例和返回值等都很详细。但就是这么一个API文档详细的平台，也存在着一些坑，在此与大家分享。

# 生成token

根据以往经验，API对接的难点有两个，一个在前期的授权，另一个是对接过程中字段的对照。这个平台授权的部分，还算是挺简单的，使用的是Basic Auth方式，只需要用户名和密码（不是登录系统的用户名和密码，是申请开发者后生成的），代码如下
```
private void GetToken()
{
    string url = "https://sts.cdiscount.com/users/httpIssue.svc/?realm=https://wsvc.cdiscount.com/MarketplaceAPIService.svc";

    HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
    request.ContentType = "application/json";
    request.Method = "GET";

    string base64Credentials = GetEncodedCredentials();
    request.Headers.Add("Authorization", "Basic " + base64Credentials);
    try
    {
        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
        string result = string.Empty;
        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        {
            result = reader.ReadToEnd();
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(result);
            Token = xml.InnerText;
        }
    }
    catch (Exception ex)
    {
        ;
    }
}

/// <summary>
/// base64Credentials
/// </summary>
/// <returns></returns>
private string GetEncodedCredentials()
{
    string mergedCredentials = string.Format("{0}:{1}", "holystonecd-api", "HS2019hs@");
    byte[] byteCredentials = UTF8Encoding.UTF8.GetBytes(mergedCredentials);
    return Convert.ToBase64String(byteCredentials);
}
```

# 具体调用
引入wcf：[https://wsvc.cdiscount.com/MarketplaceAPIService.svc](https://wsvc.cdiscount.com/MarketplaceAPIService.svc),命名为CdiscoundService，代码如下：
```
/// <summary>
/// 获取订单列表
/// </summary>
/// <param name="filter"></param>
public Order[] GetOrderList(CdiscoundService.OrderFilter filter)
{
    try
    {
        CdiscoundService.HeaderMessage headerMessage = GetHeaderMessage();
        var result = sercice.GetOrderList(headerMessage, filter);
        return result.OrderList;
    }
    catch (Exception)
    {
        return null;
    }
}

/// <summary>
/// 构造HeaderMessage参数
/// </summary>
/// <returns></returns>
private CdiscoundService.HeaderMessage GetHeaderMessage()
{
    CdiscoundService.HeaderMessage headerMessage = new CdiscoundService.HeaderMessage();
    headerMessage.Context = new CdiscoundService.ContextMessage();
    headerMessage.Context.CatalogID = 1;
    headerMessage.Context.CustomerPoolID = 1;
    headerMessage.Context.SiteID = 100;
    headerMessage.Localization = new CdiscoundService.LocalizationMessage();
    headerMessage.Localization.Country = CdiscoundService.Country.Fr;
    headerMessage.Localization.Currency = CdiscoundService.Currency.Eur;
    headerMessage.Localization.DecimalPosition = 2;
    headerMessage.Localization.Language = CdiscoundService.Language.Fr;
    headerMessage.Security = new CdiscoundService.SecurityContext();
    headerMessage.Security.TokenId = Token;
    headerMessage.Version = "1.0";
    return headerMessage;
}
```
具体参数可看：[https://dev.cdiscount.com/marketplace/?page_id=130](https://dev.cdiscount.com/marketplace/?page_id=130)

# OrderFilter参数
Cdiscount的坑就在这个参数，一不小心就掉坑里。这个参数可通过API文档先熟悉，这样对后续内容会有更深刻的理解。

## 时间条件不起作用
从文档上看，可根据创建时间BeginCreationDate和EndCreationDate查询，也可根据BeginModificationDate和EndModificationDate修改时间进行查询。然而，不管颠来倒去这两个参数怎么搞，结果还是把全部订单都查了出来。

网上对于这个平台的资料少之又少，唯一的头绪就是说States条件得合起来用。但奇迹并没有出现，订单还是全部都查出来。到此，只能暂时放弃，因为还有另一个坑。

## 查询不到订单明细
从文档上看，只需将FetchOrderLines这个值设置为true，即可获取订单明细，可实际上并非如此，这个参数怎么设都没有效果。

遇到两个坑，不得不求助Cdiscount客服，可经过2天漫长的等待，客服丢了两个PHP调用的示例，对比了一下，发现跟自己的传参并无出入。没办法，只能把入参，返回值等全部打包，再发给客服，希望能有进一步的回应。可最终等来的还是这两个PHP调用示例。

客服是没有指望了，只能自己继续研究，意外的发现了几个神秘参数，BeginCreationDateSpecified、EndCreationDateSpecified和FetchOrderLinesSpecified，分别与BeginCreationDate、EndCreationDate和FetchOrderLines一一对应，这几个参数是bool类型，这或许是每个参数的开关，于是都设置为true.奇迹真的发生了，时间条件生效，明细也出来了。

# 总结
这个接口调通后，其他接口的问题也自然都解决了。坑主要就是两个地方，一是时间条件要跟状态条件一起用，时间才能生效。二是每个参数对应的Specified字段要设置为true，对应的字段才能生效。这些在文档都没体现，客服也不会跟你说，全靠自己摸索。

[源代码](https://github.com/codernice/CdiscoundAPI.git)