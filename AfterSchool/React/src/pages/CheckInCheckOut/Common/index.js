export const setPhoneNumber = (phoneNumber, setSearchParams) => {
  setSearchParams(`phone=${encodeURIComponent(phoneNumber)}`);
};

export const getPhoneFromParams = searchParams => {
  const phone = decodeURI(searchParams.get('phone')) || '';
  return phone === 'null' ? '' : phone;
};
