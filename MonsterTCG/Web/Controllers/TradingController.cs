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
	public class TradingController
	{
		private readonly TradingService _tradingService;
		public TradingController()
		{
			_tradingService = new TradingService();
		}

		[Route("POST", "/tradings")]
		public async Task<Response> CreateTradingDeal(Request request)
		{
			try
			{
				var token = request.Headers["Authorization"];

				if (token == null)
					throw new UnauthorizedAccessException();

				var deal = JsonConvert.DeserializeObject<TradingDeal>(request.Body);

				if (await _tradingService.CreateTradingDeal(deal, token))
				{
					return new Response
					{
						StatusCode = HttpStatusCode.OK,
						Content = "Trading deal successfully created"
					};
				}
				else
				{
					return new Response
					{
						StatusCode = HttpStatusCode.Accepted,
						Content = "The deal contains a card that is not owned by the user or locked in the deck."
					};
				}
			}
			catch (PostgresException)
			{
				return new Response
				{
					StatusCode = HttpStatusCode.Conflict,
					Content = "A deal with this deal ID already exists."
				};
			}
			catch (UnauthorizedAccessException)
			{
				return new Response
				{
					StatusCode = HttpStatusCode.Unauthorized,
					Content = "Access token is missing or invalid"
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

		[Route("GET", "/tradings")]
		public async Task<Response> FetchTradingDeals(Request request)
		{
			try
			{
				var token = request.Headers["Authorization"];

				if (token == null)
					throw new UnauthorizedAccessException();

				var deals = await _tradingService.FetchTradingDeals(token);

				if(deals.Count() == 0)
				{
					return new Response
					{
						StatusCode = HttpStatusCode.NoContent,
						Content = "The request was fine, but there are no trading deals available"
					};
				}

				return new Response
				{
					StatusCode = HttpStatusCode.OK,
					Content = JsonConvert.SerializeObject(deals)
				};
			}
			catch (UnauthorizedAccessException)
			{
				return new Response
				{
					StatusCode = HttpStatusCode.Unauthorized,
					Content = "Access token is missing or invalid"
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

		[Route("DELETE", "/tradings/{tradingdealid}")]
		public async Task<Response> DeleteTradingDeal(Request request, string tradingdealid)
		{
			try
			{
				var token = request.Headers["Authorization"];

				if (token == null)
					throw new UnauthorizedAccessException();

				if (!await _tradingService.DeleteTradingDeal(token, tradingdealid))
				{
					return new Response
					{
						StatusCode = HttpStatusCode.NotFound,
						Content = "The provided deal ID was not found or the player did not create this deal."
					};
				}

				return new Response
				{
					StatusCode = HttpStatusCode.OK,
					Content = "Trading deal successfully deleted"
				};
			}
			catch (UnauthorizedAccessException)
			{
				return new Response
				{
					StatusCode = HttpStatusCode.Unauthorized,
					Content = "Access token is missing or invalid"
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

		[Route("POST", "/tradings/{tradingdealid}")]
		public async Task<Response> Trade(Request request, string tradingdealid)
		{
			try
			{
				var token = request.Headers["Authorization"];

				if (token == null)
					throw new UnauthorizedAccessException();

				var requestData = JsonConvert.DeserializeObject<string>(request.Body);

				if (!await _tradingService.Trade(token, tradingdealid, requestData))
				{
					return new Response
					{
						StatusCode = HttpStatusCode.NotFound,
						Content = "The provided deal ID was not found or the player did not create this deal."
					};
				}

				return new Response
				{
					StatusCode = HttpStatusCode.OK,
					Content = "Trading deal successfully deleted"
				};
			}
			catch (UnauthorizedAccessException)
			{
				return new Response
				{
					StatusCode = HttpStatusCode.Unauthorized,
					Content = "Access token is missing or invalid"
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
