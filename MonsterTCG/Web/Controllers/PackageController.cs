using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using MonsterTCG.Http;
using MonsterTCG.Business;
using MonsterTCG.Business.Services;
using MonsterTCG.Business.Models;
using Npgsql;
using Newtonsoft.Json;



namespace MonsterTCG.Controllers
{
	public class PackageController
	{
		private readonly PackageService _packageService;
		public PackageController()
		{
			_packageService = new PackageService();
		}

		/// <summary>
		/// Create a new package with given cards
		/// </summary>
		[Route("POST", "/packages")]
		public async Task<Response> CreatePackage(Request request)
		{
			try
			{
				var token = request.Headers["Authorization"];

				if(token == null)
				{
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized,
						Content = "Access token is missing or invalid"
					};
				}
				if(token != "Bearer admin-mtcgToken")
				{
					return new Response
					{
						StatusCode = HttpStatusCode.Forbidden,
						Content = "Provided user is not \"admin\""
					};
				}

				var requestData = JsonConvert.DeserializeObject<IEnumerable<Card>>(request.Body);

				if (await _packageService.CreatePackage(requestData))
				{
					return new Response
					{
						StatusCode = HttpStatusCode.OK,
						Content = "Package and cards successfully created"
					};
				}
				else
				{
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized,
						Content = "Invalid username/password provided"
					};
				}
			}
			catch(PostgresException)
			{
				return new Response
				{
					StatusCode = HttpStatusCode.Conflict,
					Content = "At least one card in the packages already exists"
				};
			}
			catch (Exception ex)
			{
				return new Response
				{
					StatusCode = HttpStatusCode.BadRequest,
					Content = $"The server responded with following error: {ex.Message}"
				};
			}
		}

		/// <summary>
		/// Player buys a package
		/// </summary>
		[Route("POST", "/transactions/packages")]
		public async Task<Response> BuyPackage(Request request)
		{
			try
			{
				var token = request.Headers["Authorization"];

				if (token == null)
				{
					return new Response
					{
						StatusCode = HttpStatusCode.Unauthorized,
						Content = "Access token is missing or invalid"
					};
				}

				if (await _packageService.BuyPackage(token))
				{
					return new Response
					{
						StatusCode = HttpStatusCode.OK,
						Content = "A package has been successfully bought"
					};
				}
				else
				{
					return new Response
					{
						StatusCode = HttpStatusCode.NotFound,
						Content = "No card package available for buying"
					};
				}
			}
			catch (InsufficientCoinsException ex)
			{
				return new Response
				{
					StatusCode = HttpStatusCode.Forbidden,
					Content = ex.Message
				};
			}
			catch (PostgresException)
			{
				return new Response
				{
					StatusCode = HttpStatusCode.NotFound,
					Content = "No card package available for buying"
				};
			}
			catch (Exception ex)
			{
				return new Response
				{
					StatusCode = HttpStatusCode.BadRequest,
					Content = $"The server responded with following error: {ex.Message}"
				};
			}
		}

	}
}
