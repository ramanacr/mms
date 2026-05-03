export const environment = {
  production: true,
  apiBaseUrl: 'https://your-api.example.com/api',
  auth: {
    clientId: '00000000-0000-0000-0000-000000000000',
    authority: 'https://login.microsoftonline.com/common',
    redirectUri: 'https://your-app.example.com',
    postLogoutRedirectUri: 'https://your-app.example.com/login',
    scopes: ['User.Read', 'Calendars.ReadWrite', 'People.Read'],
    apiScopes: ['api://00000000-0000-0000-0000-000000000000/access_as_user']
  }
};
