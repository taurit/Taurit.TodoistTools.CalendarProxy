# Calendar Filter

Calendar Filter is a proxy for calendars in iCalendar format that allows filtering and modification of events. It can be also applied to any calendar in iCal/vCalendar format.

## Where to find it deployed?

An example instance of this service is deployed at [https://calendar.taurit.pl/](https://calendar.taurit.pl/). There, you can preview how the filter behaves on a sample calendar and generate proxied URLs for your own calendars.

## The goal of the project

Calendar Filter gives you some control over calendars subscribed from third-party services.

It allows you to **filter out the events that you don't want to see** and **declutter the view of your daily agenda** in the calendar software or service you use.

![Calendar Filter example screenshot](https://raw.githubusercontent.com/taurit/Taurit.TodoistTools.CalendarProxy/master/screenshot.png)

Technically, the proxy is an AWS Lambda that modifies iCalendar file on the fly, according to the rules you specify.

Some examples of calendars you might want to transform before you display it as another overlay on your personal calendar are:

- Google Calendar
- Your company's Outlook calendar
- Facebook events calendar
- Meetup calendar
- Todoist calendar
- ... and all other calendars in the _ics_ format

## Calendar transformations you can use with CalendarProxy

Currently, you can:

- Shorten events that overlap the next event on this day
- Hide all-day events
- Predict event duration based on the event title and override the duration based on it (useful with _Todoist_)
- Remove recognized event duration from the event's title
- Skip tasks containing a particular substring
- Hide tasks shorter than (N) minutes
- Hide tasks from particular projects (useful with _Todoist_)
- Shorten events that are longer than (N) minutes to (M) minutes

## Development

### Website

The Angular website is maintained independently from the .NET solution. It requires Node.js 24.15.0 or newer.

From the repository root, install dependencies and start the local HTTP server:

```sh
cd Taurit.TodoistTools.CalendarProxy.UI
npm ci
npm start
```

The site is available at <http://localhost:4200/>. Create a production build with `npm run build` from the UI directory.

### .NET solution

The solution contains the shared library, its tests, and the AWS Lambda project. All projects target .NET 10; use the .NET 10 SDK to build and test them.

```sh
dotnet restore CalendarProxy.slnx
dotnet build CalendarProxy.slnx
dotnet test CalendarProxy.slnx
```

## Deployment

### Azure Website

The Azure Static Web Apps workflow builds and deploys the website automatically on pushes to `master`. It uses npm, not the .NET solution, and does not deploy the AWS backend.

### AWS backend

The **Deploy AWS Lambda** workflow automatically deploys the existing `calendarproxy` CloudFormation stack in `eu-north-1` on pushes to `master`, using the existing `tauritlambdas` artifact bucket and serverless template. It builds the solution and runs tests first, then waits for the stack update to finish. Concurrent deployments are serialized. Neither Visual Studio nor a local AWS CLI installation is required.

OIDC does not require storing or rotating AWS access keys. The IAM role and trust relationship have no scheduled expiry; GitHub obtains fresh temporary credentials automatically for every run. Their short lifetime limits exposure, not how long the integration works. This avoids a two-year credential renewal deadline, but changes to permissions, repository identity, or provider requirements can still require maintenance. .NET 10 is an LTS release; keep SDK and dependency security updates current.

Before the first deployment:

1. The GitHub environment `aws-production` has been created via GitHub CLI, allowing only the `master` branch and requiring no manual approval. View it under **Repository Settings > Environments > aws-production**.
2. Sign in to the AWS console for the account hosting `calendarproxy`. Go to **IAM > Identity providers**. If GitHub is not already listed, choose **Add provider > OpenID Connect**, enter `https://token.actions.githubusercontent.com` and audience `sts.amazonaws.com`, and save.
3. Go to **IAM > Roles > Create role > Custom trust policy**. Use the policy below, replacing `AWS_ACCOUNT_ID` with your account ID. Name the role `CalendarProxyGitHubDeploy`. The subject shown was verified for this repository; update it if you later enable immutable/custom subjects or rename the repository/environment.
4. Attach a least-privilege deployment permissions policy to that role. It must allow artifact uploads to `tauritlambdas/Taurit.TodoistTools.CalendarProxy.Serverless/` and updates to `calendarproxy` and its Lambda, API Gateway, IAM execution-role, and logging resources. Include narrowly scoped `iam:PassRole` where required. Inspect **CloudFormation > calendarproxy > Resources** and any existing stack service role to scope the policy correctly; the checked-in template alone cannot establish the full production permission set. Do not use administrator permissions as a shortcut.
5. Copy the role ARN from AWS. In **Repository Settings > Environments > aws-production > Environment variables**, add `AWS_ROLE_ARN` with that value. It is an identifier, not a credential; no AWS access-key secrets are needed. Alternatively, run the command below with your actual role ARN.
6. Commit and push to `master`. Both UI and backend pipelines run automatically and independently. Review **Actions > Deploy AWS Lambda** and smoke-test the existing calendar endpoint afterward. Deployment will fail clearly until the AWS role variable and IAM permissions are configured.

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Federated": "arn:aws:iam::AWS_ACCOUNT_ID:oidc-provider/token.actions.githubusercontent.com"
      },
      "Action": "sts:AssumeRoleWithWebIdentity",
      "Condition": {
        "StringEquals": {
          "token.actions.githubusercontent.com:aud": "sts.amazonaws.com",
          "token.actions.githubusercontent.com:sub": "repo:taurit/Taurit.TodoistTools.CalendarProxy:environment:aws-production"
        }
      }
    }
  ]
}
```

```sh
gh variable set AWS_ROLE_ARN --repo taurit/Taurit.TodoistTools.CalendarProxy --env aws-production --body "arn:aws:iam::AWS_ACCOUNT_ID:role/CalendarProxyGitHubDeploy"
```

The workflow installs the .NET 10 SDK and a pinned `Amazon.Lambda.Tools` version on an ARM64 Linux runner. It reads the checked-in deployment defaults through a temporary copy with the local named AWS profile removed, so the OIDC credentials are used. Backend application source, deployment defaults, and template are unchanged. The first cloud deployment still needs validation after IAM setup; local build checks do not prove production permissions or absence of stack drift.
