#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"
#load "fakeData.csx"

using Microsoft.WindowsAzure.Storage.Table;
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

public class IssueTableItem : TableEntity {
    public string Tags {get;set;}
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

public static void Run(TimerInfo myTimer, IQueryable<IssueTableItem> inIssueTable, ICollector<IssueTableItem> outIssueTable, ICollector<string> issueQueue, TraceWriter log)
{ 
    var issues = GetIssues(inIssueTable);
    issues.ToList().ForEach(i => issueQueue.Add(JsonConvert.SerializeObject(i)));
    CommitIssues(issues, outIssueTable);
}

public static IEnumerable<Issue> GetIssues(IQueryable<IssueTableItem> inIssueTable) {
    var existingIds = inIssueTable.Select(i => i.RowKey).ToList();
    var data = GetFakeData();
    IssueRequestResponse res = JsonConvert.DeserializeObject<IssueRequestResponse>(data);
    
    return GetDiffIssues(res.Issues, existingIds);
}

public static IEnumerable<Issue> GetDiffIssues(IEnumerable<Issue> set1, IEnumerable<string> set2Ids) {
    return set1.Where(i => !set2Ids.Contains(i.IssueId));
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

public static void CommitIssues(IEnumerable<Issue> issues, ICollector<IssueTableItem> table) {
    issues.ToList().ForEach( i => {
        table.Add(
            new IssueTableItem {
                PartitionKey = "1",
                RowKey = i.IssueId,
                Tags = String.Join(",", i.Tags)
            });
    });
}