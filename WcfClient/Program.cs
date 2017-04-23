using System;
using System.IO;
using System.Net;
using System.Xml.Linq;
using WcfClient.StudentOperationReference;
using WcfUtility;

namespace WcfClient
{
    internal static class Program
    {
        private static void Main()
        {
            var student = new Student
            {
                Age = 5,
                Name = "Omer"
            };

            //var obj = WcfSerializer.Serialize(student).ToString();
            //HttpPost(@"http://localhost:58794/StudentOperationService.svc/rest/RestStu", obj);

            using (var client = new StudentOperationServiceClient())
            {
                var returned = client.GetStudent(student);
                Console.WriteLine(returned.Name);
                Console.ReadLine();
            }
            Console.ReadLine();
        }

        private static void HttpPost(string url, string requestBody)
        {
            string res = string.Empty;

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/xml; charset=utf-8";
            //request.ContentType = @"application/json";
            //request.ContentLength = requestBody.Length;
            try
            {
                using (StreamWriter newStream = new StreamWriter(request.GetRequestStream()))
                {
                    newStream.Write(requestBody);
                }

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                ServicePointManager.Expect100Continue = true;
                string result = null;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                    }
                }

                try
                {
                    var doc = XDocument.Parse(result);
                    res += doc.ToString() + "\n";
                }
                catch (Exception)
                {
                    res = "Success";
                }
            }
            catch (WebException wex)
            {
                using (var streamReader = new StreamReader(wex.Response.GetResponseStream()))
                {
                    res = streamReader.ReadToEnd().ToString();
                }
            }
            catch (Exception ex)
            {
                res = ex.ToString();
            }
        }
    }
}