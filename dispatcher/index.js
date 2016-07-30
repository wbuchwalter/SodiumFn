var http = require('https');

module.exports = function (context, issue, tagTable) {
    context.log('dispatcher start');
     var post_options = {
      host: 'hooks.slack.com',
      port: '443',
      path: '/services/T1WKD9CK1/B1WKDL40P/UTQ6QCBRqETCJfTiObHfWCor',
      method: 'POST'
    };


   // var subscribedTags = 
    var dests = [];
    issue.tags.forEach(i => { 
        tagTable.filter(t => t.tag === i).forEach(t => {
            dests = dests.concat(JSON.parse(t.alias));
        }) 
    });

    context.log(dests);

    dests.forEach(d => {
        var req = http.request(post_options, res => {});
        req.on('error', function(e) {
            context.log('problem with request: ' + e.message);
        });

        req.write(`{"channel": "@julienstroheker", "text": " You've got some work to do! : <${issue.link}|${issue.title}>"}`);
        req.end();
    });

    context.done();
};