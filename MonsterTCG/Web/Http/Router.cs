using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MonsterTCG.Controllers;

namespace MonsterTCG.Http
{
	class Router
	{
		//Calls method for given route, methods are found in the controllers
		public async Task<Response> RouteRequestAsync(Request request)
		{
			var controllerTypes = Assembly.GetExecutingAssembly().GetTypes()
				.Where(type => type.Name.EndsWith("Controller")); //check all classes ending with Controller

			foreach (var controllerType in controllerTypes)
			{
				var methods = controllerType.GetMethods()
					.Where(m => m.GetCustomAttributes<RouteAttribute>().Any());

				foreach (var method in methods)
				{
					var attribute = method.GetCustomAttribute<RouteAttribute>();
					if (attribute == null || attribute.Method != request.Method || !MatchRoute(attribute.Path, request.Path))
					{
						continue;
					}

					var parameters = ExtractParameters(method, request);

					if (method.ReturnType == typeof(Task<Response>))
					{
						var task = method.Invoke(Activator.CreateInstance(controllerType), parameters) as Task<Response>;
						return await task;
					}
					else
					{
						return method.Invoke(Activator.CreateInstance(controllerType), parameters) as Response;
					}
				}
			}

			return new Response { StatusCode = System.Net.HttpStatusCode.NotFound, Content = "Route not found" };
		}

		//Checks route and subroute
		private bool MatchRoute(string routePattern, string requestPath)
		{
			var patternSegments = routePattern.Split('/');
			var pathSegments = requestPath.Split('/');

			if (patternSegments.Length != pathSegments.Length)
			{
				return false;
			}

			for (int i = 0; i < patternSegments.Length; i++)
			{
				if (patternSegments[i].StartsWith("{") && patternSegments[i].EndsWith("}")) //route with parameter
				{
					continue;
				}

				if (!string.Equals(patternSegments[i], pathSegments[i], StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}

			return true;
		}

		//Extracts parameters from route
		private object[] ExtractParameters(MethodInfo method, Request request)
		{
			var parameters = method.GetParameters();
			var methodParams = new object[parameters.Length];

			for (int i = 0; i < parameters.Length; i++)
			{
				var parameter = parameters[i];

				if (parameter.ParameterType == typeof(Request))
				{
					methodParams[i] = request;
					continue;
				}

				var routeAttribute = method.GetCustomAttribute<RouteAttribute>();
				var patternSegments = routeAttribute.Path.Split('/');
				var pathSegments = request.Path.Split('/');

				for (int j = 0; j < patternSegments.Length; j++)
				{
					if (patternSegments[j].StartsWith("{") && patternSegments[j].EndsWith("}") &&
						patternSegments[j].Trim('{', '}') == parameter.Name)
					{
						methodParams[i] = Convert.ChangeType(pathSegments[j], parameter.ParameterType);
						break;
					}
				}
			}

			return methodParams;
		}
	}
}
