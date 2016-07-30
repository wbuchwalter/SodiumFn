#r "Newtonsoft.Json"
#load "fakeData.csx"

using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

public class IssueRequestResponse {
    [JsonProperty("has_more")]
    public bool HasMore;

    [JsonProperty("items")]
    public IEnumerable<Issue> Issues;
}

ICollector<Person> tableBinding

public class IssueTableItem {
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string IssueId {get; set;}
}

public class Issue {
    [JsonProperty("title")]
    public string Title;

    [JsonProperty("link")]
    public string Link;

    [JsonProperty("tags")]
    public IEnumerable<string> Tags;

    [JsonProperty("question_id")]
    public string IssueId {get; set;}
}

public static void Run(TimerInfo myTimer, ICollector<string> issueQueue, ICollector<IssueTableItem> issueTable, TraceWriter log)
{ 
    var issues = GetIssues();
    issues.ToList().ForEach(i => issueQueue.Add(JsonConvert.SerializeObject(i)));
    CommitIssues(issues, issueTable);
    
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

public void CommitIssues(IEnumerable<Issue> issues, ICollector<IssueTableItem> table) {
    var items = issues.ToList().ForEach( i => {
        table.Add(
            new IssueTableItem {
                IssueId = i.IssueId,
                PartitionKey = 1
            });
    });
}