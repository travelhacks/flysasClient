using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;
using System;

namespace AwardWeb.Code
{
    public class RedirectToHttpsNoWWW : IRule
    {
        public virtual void ApplyRule(RewriteContext context)
        {
            var req = context.HttpContext.Request;
            var host = req.Host.Host;
            if (!host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                var isWWW = host.StartsWith("www.", StringComparison.OrdinalIgnoreCase);
                if (isWWW || !req.IsHttps)
                {
                    if (isWWW)
                        host = host.Substring(4);
                    var url = UriHelper.BuildAbsolute("https", new Microsoft.AspNetCore.Http.HostString(host), req.PathBase, req.Path, req.QueryString);
                    var response = context.HttpContext.Response;
                    response.StatusCode = 301;
                    response.Headers[HeaderNames.Location] = url;
                    context.Result = RuleResult.EndResponse;
                    return;
                }
            }
            context.Result = RuleResult.ContinueRules;
            return;
        }
    }
}
