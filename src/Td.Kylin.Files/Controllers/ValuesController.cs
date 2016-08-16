using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Td.Kylin.Files.Core;
using System.Text.RegularExpressions;

namespace Td.Kylin.Files.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            string file1 = "D:\\aa\\bb.jpg";

            string file2 = "D:\\aa\\bb.gif.jpg";

            string file3 = "/upload/aa.jpg";

            string file4 = "/upload/aa.gif.jpg";

            string file11 = "D:\\aa\\t_w100h100_bb.jpg";

            string file12 = "D:\\aa\\t_w100h100_bb.gif.jpg";

            string file13 = "/upload/t_w100h100_aa.jpg";

            string file14 = "/upload/t_w100h100_aa.gif.jpg";

            string file15 = "/upload/t_w100100_aa.gif.jpg";

            return new string[] {
                ThumbnailHelper.GetThumbnailPath(file1,100,100),
                ThumbnailHelper.GetThumbnailPath(file2,100,100),
                ThumbnailHelper.GetThumbnailPath(file3,100,100),
                ThumbnailHelper.GetThumbnailPath(file4,100,100),
                ThumbnailHelper.GetOrginImagePath(file11),
                ThumbnailHelper.GetOrginImagePath(file12),
                ThumbnailHelper.GetOrginImagePath(file13),
                ThumbnailHelper.GetOrginImagePath(file14),
                ThumbnailHelper.GetOrginImagePath(file15)
            };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
