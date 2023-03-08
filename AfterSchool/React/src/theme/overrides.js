export default {
  MuiLink: {
    underlineHover: {
      textDecoration: 'underline',
      '&.MuiLink-underlineHover:hover': {
        textDecoration: 'none'
      }
    }
  },
  MuiTypography: {
    root: {
      color: '#555555',
      maxWidth: '500px'
    }
  },
  MuiSelect: {
    root: {
      textAlign: 'left'
    }
  },
  MuiButton: {
    root: {
      borderRadius: '6px'
      //   minHeight: '52px'
      //   minWidth: '200px'
    },
    containedPrimary: {
      color: '#ffffff'
    }
  },
  MuiFormHelperText: {
    root: {
      '&.Mui-error': {
        color: '#d32f2f'
      }
    }
  },
  MuiOutlinedInput: {
    root: {
      '&.Mui-error .MuiOutlinedInput-notchedOutline': {
        borderColor: '#d32f2f'
      }
    },
    input: {
      '&:-webkit-autofill': {
        '-webkit-box-shadow': '0 0 0 100px #ffffff inset'
      }
    }
  }
};
