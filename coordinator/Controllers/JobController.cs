using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Karenia.Rurikawa.Coordinator.Services;
using Karenia.Rurikawa.Helpers;
using Karenia.Rurikawa.Models;
using Karenia.Rurikawa.Models.Judger;
using Karenia.Rurikawa.Models.Test;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Karenia.Rurikawa.Coordinator.Controllers {
    [ApiController]
    [Route("api/v1/job")]
    [Authorize("user")]
    public class JobController : ControllerBase {
        public JobController(ILogger<JobController> logger, DbService dbsvc, JudgerCoordinatorService coordinatorService) {
            this.logger = logger;
            this.dbsvc = dbsvc;
            this.coordinatorService = coordinatorService;
        }

        private readonly ILogger<JobController> logger;
        private readonly DbService dbsvc;
        private readonly JudgerCoordinatorService coordinatorService;

        /// <summary>
        /// GETs a job by its identifier (stringified version)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Job>> GetJob(FlowSnake id) {
            var res = await dbsvc.GetJob(id);
            if (res == null) {
                return NotFound();
            } else {
                return res;
            }
        }

        [HttpGet]
        public async Task<IList<Job>> GetJobs(
            [FromQuery] FlowSnake startId = new FlowSnake(),
            [FromQuery] int take = 20,
            [FromQuery] bool asc = false) {
            return await dbsvc.GetJobs(startId, take, asc);
        }

#pragma warning disable 
        public class NewJobMessage {
            public string Repo { get; set; }
            public string? Ref { get; set; }
            public FlowSnake TestSuite { get; set; }
            public List<string> Tests { get; set; }
        }
#pragma warning restore

        /// <summary>
        /// PUTs a new job
        /// </summary>
        [HttpPost("")]
        [Authorize("user")]
        public async Task<IActionResult> NewJob([FromBody] NewJobMessage m) {
            var account = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            FlowSnake id = FlowSnake.Generate();
            var job = new Job
            {
                Id = id,
                Account = account,
                Repo = m.Repo,
                Branch = m.Ref,
                TestSuite = m.TestSuite,
                Tests = m.Tests,
                Stage = JobStage.Queued,
            };
            try {
                var result = await GetRevision(job);
                if (result == null) {
                    return BadRequest(new ErrorResponse("No such revision"));
                } else {
                    job.Revision = result;
                }
                await coordinatorService.ScheduleJob(job);
            } catch (KeyNotFoundException) {
                return BadRequest(new ErrorResponse("No such test suite"));
            }
            return Ok(id.ToString());
        }

        /// <summary>
        /// Get the revision commit that is 
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task<string?> GetRevision(Job job) {
            System.Diagnostics.ProcessStartInfo command = new System.Diagnostics.ProcessStartInfo("git");
            command.ArgumentList.Add("ls-remote");
            command.ArgumentList.Add(job.Repo);
            command.ArgumentList.Add(job.Revision);
            command.ArgumentList.Add("-q");
            command.ArgumentList.Add("--exit-code");

            command.RedirectStandardOutput = true;

            var process = System.Diagnostics.Process.Start(command);
            var stdout = await process.StandardOutput.ReadToEndAsync();
            await Task.Run(process.WaitForExit);
            var exitCode = process.ExitCode;

            if (exitCode == 0) {
                var rev = stdout.Split('\t')[0];
                return rev;
            } else {
                return null;
            }
        }
    }
}
