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
    },
    containedPrimary: {
      color: '#ffffff'
    }
  },
  MuiFormHelperText: {
    root: {
      marginTop: 0,
      '&.Mui-error': {
        color: '#FF7070'
      }
    }
  },
  MuiPaper: {
    root: {
      width: 'fit-content'
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
        borderColor: '#FF7070'
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
