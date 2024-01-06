using System.Net;
using System.Runtime.InteropServices;
using MonsterTCG.Http;
using MonsterTCG.Business;
using MonsterTCG.Business.Services;
using MonsterTCG.Business.Models;
using Newtonsoft.Json;



namespace MonsterTCG.Controllers
{
	public class PlayerController
	{
		private readonly PlayerService _playerService;
		public PlayerController()
		{
			_playerService = new PlayerService();
		}

		/// <summary>
		/// Player login and token creation
		/// </summary>
		[Route("POST", "/sessions")]
		public async Task<Response> Login(Request request)
		{
			try
			{
				var requestData = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Body);
				var username = requestData["Username"];
				var password = requestData["Password"];

				var token = await _playerService.PlayerLogin(username, password);

				if (token != null)
				{
					return new Response
					{
						StatusCode = HttpStatusCode.OK,
						Content = token
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
		/// Return the player profile
		/// </summary>
		[Route("GET", "/users/{username}")]
		public async Task<Response> GetProfile(Request request, string username)
		{
			try
			{
				request.Headers.TryGetValue("Authorization", out var token);

				if (token == null)
					throw new UnauthorizedAccessException();

				var player = await _playerService.GetProfile(username, token);

				if (player != null)
				{
					return new Response
					{
						StatusCode = HttpStatusCode.OK,
						Content = JsonConvert.SerializeObject(new
						{
							Name = player.Name,
							Bio = player.Bio,
							Image = player.Image
						})
					};
				}
				else
				{
					return new Response
					{
						StatusCode = HttpStatusCode.NotFound,
						Content = "User not found."
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


		[Route("PUT", "/users/{username}")]
		public async Task<Response> UpdatePlayer(Request request, string username)
		{
			try
			{
				request.Headers.TryGetValue("Authorization", out var token);

				if (token == null)
					throw new UnauthorizedAccessException();

				var requestData = JsonConvert.DeserializeObject<Player>(request.Body);
				requestData.AccountName = username;

				if (await _playerService.UpdatePlayer(requestData, token))
				{
					return new Response
					{
						StatusCode = HttpStatusCode.OK,
						Content = "User sucessfully updated."
					};
				}
				else
				{
					return new Response
					{
						StatusCode = HttpStatusCode.NotFound,
						Content = "User not found."
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

		[Route("POST", "/users")]
		public async Task<Response> CreatePlayer(Request request)
		{
			try
			{
				var requestData = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Body);
				var username = requestData["Username"];
				var password = requestData["Password"];

				if (await _playerService.CreatePlayer(username, password))
				{
					return new Response
					{
						StatusCode = HttpStatusCode.Created,
						Content = "User successfully created"
					};
				}
				else
				{
					return new Response
					{
						StatusCode = HttpStatusCode.Conflict,
						Content = "User with same username already registered"
					};
				}
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
		/// Return the player stats
		/// </summary>
		[Route("GET", "/stats")]
		public async Task<Response> GetStats(Request request)
		{
			try
			{
				request.Headers.TryGetValue("Authorization", out var token);

				if (token == null)
					throw new UnauthorizedAccessException();

				var playerStats = await _playerService.GetStats(token);

				if (playerStats != null)
				{
					return new Response
					{
						StatusCode = HttpStatusCode.OK,
						Content = JsonConvert.SerializeObject(playerStats)
					};
				}
				else
				{
					return new Response
					{
						StatusCode = HttpStatusCode.NotFound,
						Content = "User not found."
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
		/// Return the scoreboard
		/// </summary>
		[Route("GET", "/scoreboard")]
		public async Task<Response> GetScoreboard(Request request)
		{
			try
			{
				request.Headers.TryGetValue("Authorization", out var token);

				if (token == null)
					throw new UnauthorizedAccessException();

				var scoreboard = await _playerService.GetScoreboard(token);

				return new Response
				{
					StatusCode = HttpStatusCode.OK,
					Content = JsonConvert.SerializeObject(scoreboard)
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


		[Route("POST", "/battles")]
		public async Task<Response> Battle(Request request)
		{
			try
			{
				request.Headers.TryGetValue("Authorization", out var token);

				if (token == null)
					throw new UnauthorizedAccessException();

				var battleResult = await _playerService.Battle(token);

				if (battleResult!=null) //battle happened
				{
					return new Response
					{
						StatusCode = HttpStatusCode.OK,
						Content = JsonConvert.SerializeObject(battleResult)
					};
				}
				else
				{
					return new Response
					{
						StatusCode = HttpStatusCode.Accepted,
						Content = "Player entered the battle queue."
					};
				}
			}
			catch (InsufficientDeckSizeException ex)
			{
				return new Response
				{
					StatusCode = HttpStatusCode.Conflict,
					Content = ex.Message
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
