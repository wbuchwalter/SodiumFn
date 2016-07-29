#r "Newtonsoft.Json"
#load "fakeData.csx"

using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;


// public class Issue {
//     public string Title;
//     public IEnumerable<string> Tags;
//     public string Link;
// }

public class IssueRequestResponse {
    [JsonProperty("has_more")]
    public bool HasMore;

    [JsonProperty("items")]
    public IEnumerable<Issue> Issues;
}

public class Issue {
    [JsonProperty("title")]
    public string Title;

    [JsonProperty("link")]
    public string Link;

    [JsonProperty("tags")]
    public IEnumerable<string> Tags;
}

public static void Run(TimerInfo myTimer, ICollector<string> issueQueue, TraceWriter log)
{ 
    var issues = GetIssues();
    issues.ToList().ForEach(i => issueQueue.Add(JsonConvert.SerializeObject(i)));
}

public static IEnumerable<Issue> GetIssues() {
    var data = GetFakeData();
    IssueRequestResponse res = JsonConvert.DeserializeObject<IssueRequestResponse>(data);
    return res.Issues;
}

public static string MakeRequest() {
    HttpWebRequest request = (HttpWebRequest)WebRequest.Create (
    "http://api.stackexchange.com/2.2/questions/no-answers?fromdate=1469145600&order=desc&sort=activity&tagged=azure&site=stackoverflow");
    request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
    WebResponse response = request.GetResponse();
    Stream dataStream = response.GetResponseStream();
    StreamReader reader = new StreamReader(dataStream);
    string responseFromServer = reader.ReadToEnd();

    reader.Close ();
    response.Close ();
    return responseFromServer;
}
