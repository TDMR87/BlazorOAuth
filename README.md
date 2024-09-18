# Blazor WebAssembly with Google Login and Individual User Accounts

### Authentication. The bane of our existence.

This project showcases how to implement external identity provider OAuth authentication and integrate it with application-specific individual user accounts in a Blazor WebAssembly application. The project is designed to serve as a practical reference, a starting point and a playground for developers looking to implement authentication to their client-side apps. The solution presented here frees you from managing the sensitive information, like users passwords, in your systems but still maintaining application-specific user accounts on your server, so that users can customize their own experience in your app. 

### Developing locally

In order to develop locally, you'll need to create your Google API credentials in the [Google Cloud Console](https://console.cloud.google.com/)'s APIs & Services. https://developers.google.com/identity/protocols/oauth2/web-server#creatingcred

Don't forget to specify the Blazor WebAssembly app's authentication callback URI in the ```Authorized Redirect URIs``` section:
> https://localhost:5001/google-login-callback 

Then put the generated ```clientId``` and ```clientSecret``` into the user secrets inside the ```BlazorOAuth.API ``` project.

>"Authentication:GoogleOptions:ClientId": "client-id-here"
>"Authentication:GoogleOptions:ClientSecret": "client-secret-here"

Start both ```BlazorOAuth.API``` and ```BlazorOAuth.Client ``` projects in their respective folders.
> dotnet run

### Architectural overview

-   **Frontend Authentication**: The Blazor WebAssembly frontend application initiates the authentication process by redirecting the user to Google’s authentication service. After the user successfully logs in with Google, Google returns an authorization code to the frontend application.
-   **Backend Exchange**: The frontend then sends this authorization code to the backend API. The backend exchanges the code with Google to retrieve an ID token, which includes user information and a unique Google ID.
-   **User Management**: The backend stores the Google ID in the application database and associates it with the user account created during the first sign-in. At the end of successful sign-in, the backend generates a JWT access token and returns it to the frontend. This token is used for communication between the ```BlazorOAuth.Client ``` and ```BlazorOAuth.API``` to ensure that only authorized requests can access protected endpoints.

![architecture_overview](assets/img/architecture_overview_diagram.png)