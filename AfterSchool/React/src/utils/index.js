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

export const formatDate = inputDate => {
  const dateObj = new Date(inputDate);
  const month = dateObj.toLocaleString('default', { month: 'long' });
  const day = dateObj.getDate();
  const year = dateObj.getFullYear();

  return `${month} ${day}, ${year}`;
};

export const changeDateTimeFormat = dateTime => {
  return new Date(dateTime).toLocaleString('en-US', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: 'numeric',
    minute: 'numeric',
    hour12: true
  });
};

export const getAndFormatDateTime = (now = new Date()) => {
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  const hours = String(now.getHours()).padStart(2, '0');
  const minutes = String(now.getMinutes()).padStart(2, '0');

  return `${year}-${month}-${day}T${hours}:${minutes}`;
};

export const getTimeFromDate = date => date.getTime();
