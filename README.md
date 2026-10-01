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

Deployment identity setup is complete in account `640775413442`:

1. The GitHub environment `aws-production` allows only the `master` branch, with no manual approval. View it under **Repository Settings > Environments > aws-production**.
2. The existing AWS OIDC provider trusts `https://token.actions.githubusercontent.com` with audience `sts.amazonaws.com`.
3. The role `CalendarProxyGitHubDeploy` uses the exact repository/environment subject in [deployment-trust.json](.github/aws/deployment-trust.json). Update this trust if the repository, environment, or OIDC subject format changes.
4. Its inline policy `CalendarProxyDeployment` is defined in [deployment-policy.json](.github/aws/deployment-policy.json), using resource IDs discovered from the live stack. The permissions policy passed IAM Access Analyzer validation. IAM simulations confirmed allowed deployment updates and denied access to unrelated stacks, functions, APIs, and artifact prefixes.
5. The environment variable `AWS_ROLE_ARN` is set to `arn:aws:iam::640775413442:role/CalendarProxyGitHubDeploy`. It is an identifier, not a credential; no AWS access-key secrets are stored in GitHub.
6. Commit and push to `master` to run both independent pipelines. Review **Actions > Deploy AWS Lambda** and smoke-test the existing calendar endpoint afterward. IAM setup alone does not verify an end-to-end OIDC login or production deployment.

The deployment policy grants artifact reads/uploads only within the existing S3 prefix, updates/change sets only on the existing stack, updates only to the existing Lambda and HTTP API, and read/tag access to the existing execution role. Managed-policy attachment is limited to `AWSLambdaBasicExecutionRole`; `iam:PassRole` permits only that execution role and only to Lambda. Template validation uses `Resource: "*"` because that action is not resource-scoped. API child resources can be maintained, but the parent API, function, and stack cannot be deleted by this policy. It does not grant IAM inline-policy editing or new stack/function/API creation.

This is an update-only deployment role, not a general infrastructure administrator. Resource replacements, new functions/APIs, layers, or changes to execution-role permissions require a reviewed policy update. No CloudFormation service role is attached to the existing stack, so resource update permissions belong to the GitHub role directly.

To reapply the checked-in policies to the existing role, use an authorized AWS administrator session and run from the repository root. These commands change IAM permissions, not the production stack:

```sh
aws iam update-assume-role-policy --role-name CalendarProxyGitHubDeploy --policy-document file://.github/aws/deployment-trust.json
aws iam put-role-policy --role-name CalendarProxyGitHubDeploy --policy-name CalendarProxyDeployment --policy-document file://.github/aws/deployment-policy.json
```

To restore the GitHub role variable if needed:

```sh
gh variable set AWS_ROLE_ARN --repo taurit/Taurit.TodoistTools.CalendarProxy --env aws-production --body "arn:aws:iam::640775413442:role/CalendarProxyGitHubDeploy"
```

The workflow installs the .NET 10 SDK and a pinned `Amazon.Lambda.Tools` version on an ARM64 Linux runner. It reads the checked-in deployment defaults through a temporary copy with the local named AWS profile removed, so the OIDC credentials are used. Backend application source, deployment defaults, and template are unchanged. The first cloud deployment still needs validation; local builds and IAM simulations do not prove absence of stack drift or complete CloudFormation resource-provider permissions.

### Identify Deployment Resources

Sign in to the AWS console for the production account, select **Europe (Stockholm)**, and open **CloudShell** from the console toolbar. CloudShell includes AWS CLI and uses your console login; no local installation or access keys are needed. Run:

```sh
aws cloudformation describe-stacks --stack-name calendarproxy --region eu-north-1 --query 'Stacks[0].{StackId:StackId,ServiceRole:RoleARN}' --output json --no-cli-pager
aws cloudformation list-stack-resources --stack-name calendarproxy --region eu-north-1 --query 'StackResourceSummaries[].{LogicalId:LogicalResourceId,Type:ResourceType,Id:PhysicalResourceId}' --output json --no-cli-pager
```

These commands only read metadata. Their output contains resource identifiers, not credentials or calendar data. Use it when reviewing policy updates after infrastructure changes. A non-null `ServiceRole` means CloudFormation uses that role for resource operations, allowing the GitHub role's permissions to focus on artifacts and stack operations rather than direct IAM/Lambda/API management. A null value means deployment must account for those resource operations separately.
