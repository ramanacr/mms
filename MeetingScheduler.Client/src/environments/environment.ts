export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:7087/api',
  auth: {
    clientId: '00000000-0000-0000-0000-000000000000',
    authority: 'https://login.microsoftonline.com/common',
    redirectUri: 'http://localhost:4200',
    postLogoutRedirectUri: 'http://localhost:4200/login',
    scopes: ['User.Read', 'Calendars.ReadWrite', 'People.Read'],
    apiScopes: ['api://00000000-0000-0000-0000-000000000000/access_as_user']
  }
};
