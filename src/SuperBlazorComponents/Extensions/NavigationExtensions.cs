using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Web;

namespace SuperBlazorComponents.Extensions;

public static class NavigationExtensions
{
	public static T? GetUrlParameterValue<T>(this NavigationManager navigationManager, string parameterName)
	{
		QueryHelpers.ParseQuery(new Uri(navigationManager.Uri).Query).TryGetValue(parameterName, out var value);
		return value == StringValues.Empty ? default : (T)Convert.ChangeType($"{value}", typeof(T));
	}

	public static string RemoveQueryStringByKey(this Uri uri, string parameterName)
	{
		var query = HttpUtility.ParseQueryString(uri.Query);
		query.Remove(parameterName);
		var path = uri.GetLeftPart(UriPartial.Path);
		return query.Count > 0 ? $"{path}?{query}" : path;
	}

	public static string AddOrUpdateQueryParam(this Uri uri, string parameterName, string value) =>
		QueryHelpers.AddQueryString(uri.RemoveQueryStringByKey(parameterName), parameterName, value);
}
