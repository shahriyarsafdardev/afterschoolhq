export default {
  navigateTo: path => {
    window.location.href = path;
  },
  scrollToTop: () => window.scrollTo(0, 0),
  redirect: link => window.location.replace(link),
  getEnv: () => {
    const envs = {
      ['staging.afterschoolhq.com']: 'Staging',
      ['afterschoolhq.com']: 'Production',
      ['qa.afterschoolhq.com']: 'QA',
      ['localhost:54849']: 'Local'
    };
    return envs[window.location.host];
  }
};
