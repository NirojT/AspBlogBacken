using Application.Blog;
using Domain.Blog.dto;
using Domain.Blog.entity;
using Infrastructure.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Presentation.Blog.utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Blog.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class ReactController : ControllerBase
    {
        private readonly ILogger<ReactController> _logger;
        private readonly IReactService _reactService;
        private readonly IHubContext<SignalRNoti> _signalRNoti;

        // Constructor injection for ILogger, IReactService, and IHubContext
        public ReactController(ILogger<ReactController> logger, IReactService reactService, IHubContext<SignalRNoti> signalRNoti)
        {
            _logger = logger;
            _reactService = reactService;
            _signalRNoti = signalRNoti;
        }

        // Method for handling server errors
        private IActionResult serverError(Exception ex)
        {
            return BadRequest(new { status = 500, message = "in React error is " + ex.Message });
        }

        // API endpoint for creating a react
        [HttpPost("create")]
        public async Task<IActionResult> createReact([FromBody] ReactDto reactDto)
        {
            try
            {
                // Call the service method to create a react
                Task<string> isCreated = _reactService.createReact(reactDto);
                string result = isCreated.Result;

                // Send notification if the react is created
                if (result != null) await _signalRNoti.Clients.All.SendAsync("notis", result);

                // Return appropriate response based on the result
                return result != null
                    ? Ok(new { status = 200, message = "React created successfully" })
                    : BadRequest(new { status = 400, message = "Failed to create React" });
            }
            catch (Exception ex)
            {
                // Handle exceptions and return server error response
                return serverError(ex);
            }
        }

        // API endpoint for updating a react
        [HttpPut("update/{react_id}")]
        public IActionResult updateReact([FromBody] ReactDto reactDto, Guid react_id)
        {
            try
            {
                // Call the service method to update a react
                Task<bool> isUpdated = _reactService.updateReact(reactDto, react_id);
                bool result = isUpdated.Result;

                // Return appropriate response based on the result
                return result
                    ? Ok(new { status = 200, message = "React updated successfully" })
                    : BadRequest(new { status = 400, message = "Failed to update React" });
            }
            catch (Exception ex)
            {
                // Handle exceptions and return server error response
                return serverError(ex);
            }
        }

        // API endpoint for retrieving all reacts
        [HttpGet, Route("get-all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllReact()
        {
            try
            {
                // Call the service method to get all reacts
                List<React> reacts = await _reactService.getAllReacts();

                // Return response with the list of reacts
                return base.Ok(new
                {
                    status = reacts.Count > 0 ? 200 : 400,
                    data = reacts.Count > 0 ? reacts : new List<React>()
                });
            }
            catch (Exception ex)
            {
                // Handle exceptions and return server error response
                return serverError(ex);
            }
        }

        // API endpoint for retrieving a react by its ID
        [HttpGet("get/{react_id}")]
        public IActionResult getReactbyId(ReactDto reactDto, Guid react_id)
        {
            try
            {
                // Call the service method to get a react by its ID
                Task<React> reacts = _reactService.getReactById(reactDto, react_id);
                React react = reacts.Result;

                // Return response with the react data
                return react != null
                    ? Ok(new { status = 200, data = react })
                    : BadRequest(new { status = 400, data = new React() });
            }
            catch (Exception ex)
            {
                // Handle exceptions and return server error response
                return serverError(ex);
            }
        }

        // API endpoint for deleting a react by its ID
        [HttpDelete("delete/{react_id}")]
        public IActionResult deleteReact(Guid react_id, string userId, Guid? blogId, Guid? commentId)
        {
            try
            {
                // Create ReactDto object with user ID, blog ID, and comment ID
                ReactDto reactDto = new ReactDto { userId = userId, blogId = blogId, commentId = commentId };

                // Call the service method to delete a react
                Task<bool> task = _reactService.deleteReact(reactDto, react_id);
                bool result = task.Result;

                // Return appropriate response based on the result
                return result
                    ? Ok(new { status = 200, message = "React deleted successfully" })
                    : BadRequest(new { status = 400, message = "Failed to delete React" });
            }
            catch (Exception ex)
            {
                // Handle exceptions and return server error response
                return BadRequest(new { error = ex.Message });
            }
        }

        // API endpoint for retrieving the count of reacts
        [HttpGet("get-count")]
        public IActionResult getReactCounts()
        {
            try
            {
                // Call the service method to get the count of reacts
                Task<object> task = _reactService.getNoOfReact();
                object result = task.Result;

                // Return response with the count of reacts
                return Ok(new { status = 200, data = result, message = "Reacts found" });
            }
            catch (Exception ex)
            {
                // Handle exceptions and return server error response
                return serverError(ex);
            }
        }

        // API endpoint for retrieving the count of reacts within a specified date range
        [HttpGet("get-countByDate")]
        public IActionResult getNoOfReactByDate(string from, string to)
        {
            try
            {
                // Call the service method to get the count of reacts within the specified date range
                Task<object> reactCountTask = _reactService.getNoOfReactByDate(from, to);
                object result = reactCountTask.Result;

                // Return response with the count of reacts within the date range
                return Ok(new { status = 200, data = result, message = "Reacts found" });
            }
            catch (Exception ex)
            {
                // Handle exceptions and return server error response
                return serverError(ex);
            }
        }
    }
}
