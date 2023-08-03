const emailValidationRegex = /^([a-zA-Z0-9_\-.+]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,63}))$/;
export const phoneValidationRegex = /^(?:(?:\+?1\s*(?:[.-]\s*)?)?(?:(\s*(\(?([2-9]{1})([0-9]{2}))\)?\s*)|([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9]))\s*(?:[.-]\s*)?)([2-9]1[02-9]|[2-9][02-9]1|[2-9][02-9]{2})\s*(?:[.-]\s*)?([0-9]{4})$/;
export const numericValidationRegex = /(?=.*?\d)^\$?(([1-9]\d{0,2}(,\d{3})*)|\d+)?(\.\d{1,2})?$/;

export default {
  validateEmail: value => emailValidationRegex.test(value),
  validatePhone: value => phoneValidationRegex.test(value)
};
