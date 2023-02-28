export default {
  navigateTo: path => {
    window.location.href = path;
  },
  scrollToTop: () => window.scrollTo(0, 0),
  redirect: link => window.location.replace(link)
};
