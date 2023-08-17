import { useDebouncedCallback } from 'use-debounce';

const DEBOUNCE_TIME = 400;

export default {
  pathWithOrigin: path => `${window.location.origin}${path}`,
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
    return envs[window.location.host] || 'Local';
  }
};

export const Debounce = (func, time = DEBOUNCE_TIME) => useDebouncedCallback(func, time);

export const buildQueryParams = (paramName, paramValue, searchParams) => {
  const queryParams = [];
  const newQueryParam = `${paramName}=${paramValue}`;
  for (const [key, value] of searchParams.entries()) {
    if (key === paramName && Boolean(paramValue)) queryParams.push(newQueryParam);
    else if (key !== paramName) queryParams.push(`${key}=${value}`);
  }

  if (!queryParams.includes(newQueryParam) && Boolean(paramValue)) queryParams.push(newQueryParam);
  return queryParams.join('&');
};
