import utils from 'utils';
const { getEnv } = utils;

const rollbarConfig = {
  accessToken: process.env.ROLLBAR_CLIENT_KEY,
  environment: getEnv()
};
export default rollbarConfig;
