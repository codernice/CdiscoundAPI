using GS.Cdiscount.CdiscoundService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GS.Cdiscount
{
    public class CdiscountClient
    {
        private string Token = "";
        CdiscoundService.MarketplaceAPIService sercice = new CdiscoundService.MarketplaceAPIService();

        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <param name="filter"></param>
        public Order[] GetOrderList(CdiscoundService.OrderFilter filter)
        {
            try
            {
                GetToken();
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
        /// 获取店铺信息
        /// </summary>
        public SellerMessage GetSellerInformation()
        {
            try
            {
                CdiscoundService.HeaderMessage headerMessage = GetHeaderMessage();
                var result = sercice.GetSellerInformation(headerMessage);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 构造HeaderMessage参数（具体可参考API文档）
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

        /// <summary>
        /// 获取token
        /// </summary>
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
            //使用的是后台创建完app后生成的用户名和密码
            string mergedCredentials = string.Format("{0}:{1}", "用你的用户名替换", "用你的密码替换");
            byte[] byteCredentials = UTF8Encoding.UTF8.GetBytes(mergedCredentials);
            return Convert.ToBase64String(byteCredentials);
        }
    }
}
