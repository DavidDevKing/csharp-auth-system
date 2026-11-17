# Csharp Auth System

> An authentication system for a "model" company.

## How It Works

This project simulates an imaginary company's employee onboarding. Ideally the emails would belong to the company's domain, but that is not simulated in the project.

### Application Workflow

1. Admin creates account.
2. System sends email to Employee.
3. Employee activates account through the invitation token sent in the email.

> **Note:** Accounts not activated before their invitation expires (24 hours by default, set by `Invitation.ExpiryMinutes`) are removed by a cleanup job that runs every 6 hours, so an expired account can remain for up to 6 hours before deletion.

> **Note:** The invitation email sent is supposed to have a link to the front-end with the token as a query, but there is no front-end so it's a placeholder link. If you want to test the verify token and accept token endpoints, you'd have to manually copy the token from the link and include it in the request body.

## Tech Stack

- `C#` programming language
- `.NET`
- `postgresql`
- `docker`
- `ASP.NET Core`
- `Entity Framework Core`
- `JWT`
- `BCrypt`

## Dependencies

- `dotnet-host 10.0+`
- `dotnet-sdk 10.0+`
- `dotnet-runtime 10.0+`
- `docker 29.0+`
- `docker-compose 5.0+`

## Getting Started

1. Install the `EF Core` CLI tool.

   ``` bash
   dotnet tool install --global dotnet-ef
   ```

2. Configure `.env`/`.env.development` and `database.env` files using `example.env` and `example.database.env` as guides.

3. Set the env files for the auth system container and the postgres container in the `docker-compose.yml` file.

   ``` yaml
   db:
     image: postgres:18
     container_name: auth_db
     restart: unless-stopped
     env_file: ./Environment/database.env # Change this if you use a different naming convention
   ```

   ``` yaml
   sys_local:
     container_name: auth_system_watch
     build:
       context: .
       dockerfile: dockerfile.dev
     restart: unless-stopped
     ports: 
       - "4000:5227"
     volumes:
       - .:/app
       - /app/bin
       - /app/obj
     env_file: ./Environment/.env.development # Change this to .env if you're using that instead of .env.development.
   ```

4. Start the database service *(edit this if you've changed the name of the service)*.

   ``` bash
   docker compose up -d db
   ```

5. Install the necessary packages.

   ``` bash
   dotnet restore
   ```

6. Apply the database migration.

   ``` bash
   dotnet ef database update
   ```

7. Start the docker containers.

   ``` bash
   docker compose up --build -d
   ```

8. On first startup, a seeding script creates the initial admin account from the credentials in your `.env` or `.env.development` file. Use the credentials you configured to log in.

> **Note:** The default system port in the configuration is `4000`. You can always change this in the `docker-compose.yml` file.

## API Endpoints

| Endpoint                                       |  Method  |  Role   | Description                                                                                               | Response Example                                                                                                                                          |
| :--------------------------------------------- | :------: | :-----: | :-------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `/api/auth/admin/users/{id}`                   |  `GET`   | `ADMIN` | Gets a user's information based on their id.                                                              | `{"id": 2,"firstName": "John", "lastName": "Doe", "email": "user@example.com", "role": 0, "isActive": false, "createdAt": "2026-10-12T13:11:00+01:00"}`   |
| `/api/auth/admin/users`                        |  `GET`   | `ADMIN` | Returns a list of users with configurations that match the query.                                         | `[{"id": 2,"firstName": "John", "lastName": "Doe", "email": "user@example.com", "role": 0, "isActive": false, "createdAt": "2026-10-12T13:11:00+01:00"}]` |
| `/api/auth/admin/users`                        |  `POST`  | `ADMIN` | Creates a new user and sends an invitation email. Includes a `warnings` array if the email fails to send. | `{"id": 2,"firstName": "John", "lastName": "Doe", "email": "user@example.com", "role": 0, "isActive": false, "createdAt": "2026-10-12T13:11:00+01:00"}`   |
| `/api/auth/admin/users/{id}/resend-invitation` |  `POST`  | `ADMIN` | Re-sends an invitation email to the user with the specified id.                                           | `No Content`                                                                                                                                              |
| `/api/auth/admin/users/{id}`                   | `DELETE` | `ADMIN` | Deletes the user with the specified id.                                                                   | `No Content`                                                                                                                                              |
| `/api/auth/login`                              |  `POST`  |  `ANY`  | Generates access and refresh tokens and logs the user in.                                                 | `No Content`                                                                                                                                              |
| `/api/auth/logout`                             |  `POST`  |  `ANY`  | Deletes access and refresh tokens and logs the user out.                                                  | `No Content`                                                                                                                                              |
| `/api/auth/refresh`                            |  `POST`  |  `ANY`  | Refreshes expired access tokens.                                                                          | `No Content`                                                                                                                                              |
| `/api/auth/invitations/verify`                 |  `POST`  |  `ANY`  | Verifies the user with the invitation token exists, and returns the user's details.                       | `{"id": 2,"firstName": "John", "lastName": "Doe", "email": "user@example.com", "role": 0}`                                                                |
| `/api/auth/invitations/accept`                 |  `POST`  |  `ANY`  | Sets the user's password and activates the account.                                                       | `No Content`                                                                                                                                              |

## Examples

### Login

``` bash
curl -X POST http://localhost:4000/api/auth/login \
-H "Content-Type: application/json" \
-c cookies.txt \
-d '{"email": "user@example.com", "password": "my_password"}'
```

### Create a User (201 Created)

``` bash
curl -X POST http://localhost:4000/api/auth/admin/users \
-H "Content-Type: application/json" \
-b cookies.txt \
-d '{"firstName": "John", "lastName": "Doe", "email": "user@example.com", "role": 0}'
```

### Verify Invitation Token

``` bash
curl -X POST http://localhost:4000/api/auth/invitations/verify \
-H "Content-Type: application/json" \
-d '{"token": "INVITATION_TOKEN"}'
```

### Activate Account

``` bash
curl -X POST http://localhost:4000/api/auth/invitations/accept \
-H "Content-Type: application/json" \
-d '{"token": "INVITATION_TOKEN", "password": "my_new_password"}'
```

### Find Users by Query

``` bash
curl -X GET "http://localhost:4000/api/auth/admin/users?FirstName=John&LastName=Doe&IsActive=true" \
-b cookies.txt
```

#### Available query parameters

``` csharp
string? FirstName;
string? LastName;
string? Email;
int? Role; //Must be 0:USER, 1:MANAGER or 2:ADMIN
bool? IsActive;
DateTime? CreatedBefore;
DateTime? CreatedAfter;
```

> **Note:** Admin endpoints use cookie authentication. Log in once with `-c cookies.txt` to save your session cookies, then pass `-b cookies.txt` on later requests.
> **Note:** These are all placeholder values. Replace them with the values configured in the `.env` or `.env.development` file.

## Remarks

- This is a development server for now and as such, make use of `dotnet watch run` in the docker container. Plans to turn this into a production server are in the works.
- The user's refresh tokens are stored in the database, but are not being used. Plans to implement stateful sessions are in the works.

## License

[MIT](LICENSE)
