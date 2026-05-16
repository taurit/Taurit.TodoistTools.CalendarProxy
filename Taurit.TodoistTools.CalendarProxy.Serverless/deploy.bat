REM Install or update the AWS Lambda global tools to support your .NET version
dotnet tool update -g Amazon.Lambda.Tools

REM Deploy your Lambda function
dotnet lambda deploy-serverless