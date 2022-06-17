using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using TestDemoFileWithAmazonS3.Models;

namespace TestDemoFileWithAmazonS3.Controllers
{
    [Route("Api/files")]
    [ApiController]
    public class FileController : Controller
    {
        private readonly IAmazonS3 _s3Client;

        public FileController(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        // Upload file in Amazon S3 
        // file is the name of the file 
        // bucketname is the specific bucket name in which file will go 
        // folder is folder inside the bucket to store files

        [HttpPost("upload")]
        public async Task<IActionResult> FileUploadAsync(List<IFormFile> files, string? folder, string? subfolder )
        {
            foreach (var file in files)
            {
                // supported types extention verification
                var supportedTypes = new[] { "txt", "doc", "docx", "pdf", "xls", "xlsx" };
                var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);

               // var bucketExist = await _s3Client.DoesS3BucketExistAsync(bucketname);
                //if bucket  does not exists.. return exception 
               // if (!bucketExist) { return NotFound($"Bucket {bucketname} does not exist."); }
                // else insertion process 
               // else
                {
                    //if file does not support the types needed return badrequest
                    if (!supportedTypes.Contains(fileExt))
                    {
                        return BadRequest($"file is not among this types txt/doc/docx/pdf/xls/xlsx ...");
                    }
                    // else upload file to the specified bucket inside the specified folder
                    else
                    {
                        var request = new  PutObjectRequest()
                        {
                            BucketName = "asdfghjklqwert",
                            Key = string.IsNullOrEmpty(folder) ? file.FileName : $"{folder}/{subfolder?.TrimEnd('/')}/{file.FileName}",                           
                            InputStream = file.OpenReadStream()
                        };
                        //meta data configuration
                        request.Metadata.Add("Content-type", file.ContentType);
                        // store in the folder
                        await _s3Client.PutObjectAsync(request);
                        
                    }
                }
            }
            return Ok($"files uploaded to s3 sucessfully in folder:  {folder} ");
        }

        //get all bucket file using bucket folder address 

        [HttpGet("Get-All")]
        public async Task<IActionResult> GetAllFilesAsync(string bucketname , string? folder)
        {
            var bucketExist = await _s3Client.DoesS3BucketExistAsync(bucketname);
            //if bucket  does not exists.. return exception 
            if (!bucketExist) { return NotFound($"bucket {bucketname} does not exist..."); }
            //for listing object navigation i.e (bucketname/folder)
            var request = new ListObjectsV2Request()
            {
                BucketName = bucketname,
                Prefix = folder
            };
            // request path
            var result = await _s3Client.ListObjectsV2Async(request);
            // select and display the path and the link to display the content stored 
            var s3Objects = result.S3Objects.Select(s =>
            {
                var urlRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = bucketname,
                    Key = s.Key,
                    Expires = DateTime.UtcNow.AddSeconds(30), // link will work till 30 secs
                };
                return new S3ObjectAccess()
                {
                    Name = s.Key.ToString(),
                    PreSignedURL = _s3Client.GetPreSignedURL(urlRequest)
                };
            });
            return Ok(s3Objects);
        }

        // not importent (only usefull when needed download option on client side)

        [HttpGet("Get-by-key")]
        public async Task<IActionResult> DownloadFileByKeyAsync (string bucketname, string key)
        {
            var bucketExist = await _s3Client.DoesS3BucketExistAsync(bucketname);
            //if bucket  does not exists.. return exception 
            if (!bucketExist) { return NotFound($"bucket {bucketname} does not exist..."); }
            // accept the key as an input i.e (foldername/filename.extention which you want to display)
            var s3Object = await _s3Client.GetObjectAsync(bucketname, key);
            return File(s3Object.ResponseStream, s3Object.Headers.ContentType);
        }

        // delete file from the specific folder or with folder

        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteFileFromBucketAsync( string key)
        {
            //var bucketExist = await _s3Client.DoesS3BucketExistAsync(bucketname);
            //if bucket  does not exists.. return exception 
            //if (!bucketExist) { return NotFound($"bucket {bucketname} does not exist..."); }

            // delete file to the specific navigation i.e (folder/filename.extention)
            await _s3Client.DeleteObjectAsync("asdfghjklqwert", key);
            return NoContent();
        }
    }
}
