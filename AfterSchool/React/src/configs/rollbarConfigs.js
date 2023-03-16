const rollbarConfig = {
  accessToken: process.env.ROLLBAR_CLIENT_KEY,
  environment: process.env.ENVIRONMENT
};
export default rollbarConfig;
