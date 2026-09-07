using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MyMeet.Api;

[Authorize]
public class SignalingHub : Hub
{

}
