using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Motix.Models;

namespace Motix.Services;

public static class LinkFactory
{
    public static List<Link> SectorLinks(HttpContext ctx, Guid id)
    {
        var url = (LinkGenerator)ctx.RequestServices.GetService(typeof(LinkGenerator))!;
        return new()
        {
            new("self",   url.GetUriByAction(ctx,"GetById","Sectors",new { id })!, "GET"),
            new("update", url.GetUriByAction(ctx,"Put","Sectors",new { id })!, "PUT"),
            new("delete", url.GetUriByAction(ctx,"Delete","Sectors",new { id })!, "DELETE"),
        };
    }

    public static List<Link> MotorcycleLinks(HttpContext ctx, Guid id)
    {
        var url = (LinkGenerator)ctx.RequestServices.GetService(typeof(LinkGenerator))!;
        return new()
        {
            new("self",   url.GetUriByAction(ctx,"GetById","Motorcycles",new { id })!, "GET"),
            new("update", url.GetUriByAction(ctx,"Put","Motorcycles",new { id })!, "PUT"),
            new("delete", url.GetUriByAction(ctx,"Delete","Motorcycles",new { id })!, "DELETE"),
        };
    }

    public static List<Link> MovementLinks(HttpContext ctx, Guid id)
    {
        var url = (LinkGenerator)ctx.RequestServices.GetService(typeof(LinkGenerator))!;
        return new()
        {
            new("self",   url.GetUriByAction(ctx,"GetById","Movements",new { id })!, "GET"),
            new("delete", url.GetUriByAction(ctx,"Delete","Movements",new { id })!, "DELETE"),
        };
    }
}