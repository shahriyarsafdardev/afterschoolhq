import utils from 'utils';
const { getEnv } = utils;

const rollbarConfig = {
  captureUncaught: true,
  captureUnhandledRejections: true,
  accessToken: process.env.ROLLBAR_CLIENT_KEY,
  payload: {
    environment: getEnv(),
    client: {
      javascript: {
        guess_uncaught_frames: true,
        code_version: process.env.CODE_VERSION
      }
    }
  }
};
export default rollbarConfig;
