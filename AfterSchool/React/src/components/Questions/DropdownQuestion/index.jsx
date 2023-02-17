import React from 'react';
import { Box, Select, FormControl, FormHelperText, MenuItem } from '@material-ui/core';
import FieldWrapper from '../../FieldWrapper/FieldWrapper';

import useStyles from './styles';

function DropdownQuestion({ question, formik }) {
  const classes = useStyles();
  const { touched, values, errors, setFieldValue } = formik;

  const { ID, Options, Text: title, IsRequired: isRequired = false } = question;
  const options = Options.split(',').map(option => ({ key: option, value: option }));

  const handleChange = ({ target: { value } }) => {
    const selectedOption = options.find(option => option.value === value);
    if (selectedOption) {
      setFieldValue(ID, selectedOption.value);
    }
  };

  return (
    <>
      <FieldWrapper label={title} isRequired={isRequired}>
        <div className={classes.wrapper}>
          <div className={classes.dropdownWrapper}>
            <div className={classes.formControlClass}>
              <FormControl
                variant="outlined"
                classes={{
                  root: classes.formControlRoot
                }}
              >
                <Select
                  id="dropdown-question"
                  value={values[ID] || 'Select'}
                  onChange={handleChange}
                  classes={{
                    select: classes.select
                  }}
                  MenuProps={{
                    anchorOrigin: {
                      vertical: 'bottom',
                      horizontal: 'left'
                    },
                    transformOrigin: {
                      vertical: 'top',
                      horizontal: 'left'
                    },
                    getContentAnchorEl: null
                  }}
                >
                  <MenuItem value="Select">Select</MenuItem>
                  {options.length &&
                    options.map(option => (
                      <MenuItem
                        key={option.key}
                        value={option.value}
                        classes={{
                          root: classes.list
                        }}
                      >
                        {option.value}
                      </MenuItem>
                    ))}
                </Select>
              </FormControl>
            </div>
          </div>
        </div>
      </FieldWrapper>
      {touched[ID] && (
        <Box className={classes.flexCenter}>
          <FormHelperText className={classes.maxWidth} error>
            {errors[ID]}
          </FormHelperText>
        </Box>
      )}
    </>
  );
}

export default DropdownQuestion;
