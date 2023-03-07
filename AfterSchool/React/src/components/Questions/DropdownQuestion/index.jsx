import React from 'react';
import { Box, Select, FormControl, FormHelperText, MenuItem } from '@material-ui/core';
import FieldWrapper from 'components/FieldWrapper/FieldWrapper';

import useStyles from './styles';

function DropdownQuestion({ question, formik }) {
  const classes = useStyles();
  const { touched, values, errors, setFieldValue } = formik;

  const { ID, Options, Text: title, IsRequired: isRequired = false } = question;
  const options = Options.split(',').map(option => {
    const [key, value] = option.includes('|') ? option.split('|') : [option, option];
    return { key, value };
  });

  const handleChange = ({ target: { value } }) => {
    const selectedOption = options.find(option => option.value === value);
    setFieldValue(ID, selectedOption ? JSON.stringify([selectedOption]) : '');
  };

  const value = values[ID] && values[ID] != 'null' ? JSON.parse(values[ID])[0]?.value : 'Select';

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
                  value={value}
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
                  <MenuItem key="Select" value="Select">
                    Select
                  </MenuItem>
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
