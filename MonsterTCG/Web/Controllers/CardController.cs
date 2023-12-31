using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using MonsterTCG.Http;
using MonsterTCG.Business;
using MonsterTCG.Business.Services;
using MonsterTCG.Business.Models;
using Npgsql;
using Newtonsoft.Json;
using System.Numerics;
using MonsterTCG.Business.Database;



namespace MonsterTCG.Controllers
{
	public class CardController
	{
		private readonly PlayerRepository _playerRepository;
		private readonly CardService _cardService;
		public CardController()
		{
			_cardService = new CardService();
			_playerRepository = new PlayerRepository();
		}

		/// <summary>
		/// Return the player stack
		/// </summary>
		[Route("GET", "/cards")]
		public async Task<Response> GetStack(Request request)
		{
			try
			{
				request.Headers.TryGetValue("Authorization", out var token);

				if (token == null)
					throw new UnauthorizedAccessException();

				var tokenPlayer = await _playerRepository.GetPlayer(token);
				if (tokenPlayer == null)
					throw new UnauthorizedAccessException();

				var stack = await _cardService.GetStack(token);

				if (stack.Count == 0)
				{
					return new Response
					{
						StatusCode = HttpStatusCode.NoContent,
						Content = "The request was fine, but the user doesn't have any cards"
					};
				}
				else
				{
					return new Response
					{
						StatusCode = HttpStatusCode.OK,
						Content = JsonConvert.SerializeObject(stack)
					};
				}
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

		/// <summary>
		/// Return the player deck
		/// </summary>
		[Route("GET", "/deck")]
		public async Task<Response> GetDeck(Request request)
		{
			try
			{
				request.Headers.TryGetValue("Authorization", out var token);

				if (token == null)
					throw new UnauthorizedAccessException();

				var tokenPlayer = await _playerRepository.GetPlayer(token);
				if (tokenPlayer == null)
					throw new UnauthorizedAccessException();

				var stack = await _cardService.GetDeck(token);

				if (stack.Count == 0)
				{
					return new Response
					{
						StatusCode = HttpStatusCode.NoContent,
						Content = "The request was fine, but the user doesn't have any cards"
					};
				}
				else
				{
					return new Response
					{
						StatusCode = HttpStatusCode.OK,
						Content = JsonConvert.SerializeObject(stack)
					};
				}
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

		/// <summary>
		/// Sets the player deck
		/// </summary>
		[Route("PUT", "/deck")]
		public async Task<Response> SetDeck(Request request)
		{
			try
			{
				request.Headers.TryGetValue("Authorization", out var token);

				if (token == null)
					throw new UnauthorizedAccessException();

				var tokenPlayer = await _playerRepository.GetPlayer(token);
				if (tokenPlayer == null)
					throw new UnauthorizedAccessException();

				var cardIds = JsonConvert.DeserializeObject<IEnumerable<string>>(request.Body);

				if(cardIds.Count() != 4)
				{
					return new Response
					{
						StatusCode = HttpStatusCode.BadRequest,
						Content = "The provided deck did not include the required amount of cards"
					};
				}

				if(await _cardService.SetDeck(cardIds,token))
				{
					return new Response
					{
						StatusCode = HttpStatusCode.OK,
						Content = "The deck has been successfully configured"
					};
				}
				else
				{
					return new Response
					{
						StatusCode = HttpStatusCode.Forbidden,
						Content = "At least one of the provided cards does not belong to the user or is not available."
					};
				}

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
