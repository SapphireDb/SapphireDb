export interface RealtimeDatabaseOptions {
  serverBaseUrl: string;
  useSecuredSocket?: boolean;
  secret?: string;

  loginRedirect?: string;
  unauthorizedRedirect?: string;
}
