export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:7087/api',
  auth: {
    clientId: '0a604aae-9fbf-4656-93f0-ebd7a3b1be62', // Replace with your Azure AD app registration client ID
    authority: 'https://login.microsoftonline.com/common',
    redirectUri: 'http://localhost:52264',
    postLogoutRedirectUri: 'http://localhost:52264',
    scopes: ['User.Read', 'Calendars.ReadWrite', 'People.Read'],
    apiScopes: ['api://0a604aae-9fbf-4656-93f0-ebd7a3b1be62/access_as_user'] // Replace with your API scope
  }
};
