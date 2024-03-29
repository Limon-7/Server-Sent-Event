﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerSentEvent.Entities;
using ServerSentEvent.Services;
using System.Threading.Tasks;

namespace ServerSentEvent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IServerSentEventService service;

        public NotificationsController(IServerSentEventService service)
        {
            this.service = service;
        }

        [HttpGet("Connect")]
        public async Task<IActionResult> ConnectAsync()
        {
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            return Ok(new { status= "Connected" });
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ServerSentEventMessage message)
        {
            if (string.IsNullOrEmpty(message?.Id) ||
                string.IsNullOrEmpty(message?.Message))
            {
                return Ok("No messages");
            }
            await service.SendMessageAsync(message);
            return Ok();
        }
    }
}
