export default {
  MuiButton: {
    root: {
      borderRadius: '6px'
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
