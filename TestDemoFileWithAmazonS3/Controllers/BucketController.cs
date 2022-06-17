using Amazon.S3;
using Microsoft.AspNetCore.Mvc;

namespace TestDemoFileWithAmazonS3.Controllers
{
    [Route("Api/buckets")]
    [ApiController]
    public class BucketController : Controller
    {
       private readonly IAmazonS3 _s3Client;

        public BucketController(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        // this method is use to get aa the bucket exists in amazon S3

        [HttpGet("bucketlist")]
        public async Task<IActionResult> GetAllBucketListAsync()
        {
            var data = await _s3Client.ListBucketsAsync();
            var buckets = data.Buckets.Select(b => { return b.BucketName; });
            return Ok(buckets);
        }

        // this method is use to create new bucket in Amazon s3 by bucketname

        [HttpPost("create")]
        public async Task<IActionResult> CreateBucketAsync (string bucketname)
        {
            var bucketExist = await _s3Client.DoesS3BucketExistAsync(bucketname);
            // if bucket exist return it is already exists 
            if (bucketExist) { return BadRequest($"bucket {bucketname} already exists..."); }
            // if bucket not exists create new bucket with bucket name 
            await _s3Client.PutBucketAsync(bucketname);
            return Ok($"Bucket {bucketname} created.");
        }

        // this method is used to delete existing buckets by bucket name

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteBucketAsync(string bucketname)
        {
            await _s3Client.DeleteBucketAsync(bucketname);
            return NoContent();
        }
    }
}
