export default {
  MuiChip: {
    root: {
      fontSize: '14px',
      minWidth: '100px'
    }
  },
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
      color: '#555555'
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
  MuiPaper: {
    root: {
      width: 'fit-content',
      padding: '0px 15px'
    }
  },
  MuiDialogTitle: {
    root: {
      padding: '16px 0px !important'
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
  },
  MuiInputBase: {
    root: { '&.Mui-disabled': { color: 'gray' } }
  }
};
