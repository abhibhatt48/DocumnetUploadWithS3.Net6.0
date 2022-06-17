namespace TestDemoFileWithAmazonS3.Models
{

    // for generation of the link to see the uploaded file from AWS console
    public class S3ObjectAccess
    {
        public string? Name { get; set; }

        public string? PreSignedURL { get; set; }
    }
}
