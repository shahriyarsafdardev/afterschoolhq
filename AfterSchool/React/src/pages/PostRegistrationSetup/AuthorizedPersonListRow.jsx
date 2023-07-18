import React from 'react';
import CheckIcon from '@material-ui/icons/Check';
import { Checkbox, FormControlLabel, Box, Typography } from '@material-ui/core';
import useStyles from './styles';
import { ErrorBand } from 'components';

function AuthorizedPersonListRow({ handleChange, touched, person, errors, handleEdit }) {
  const classes = useStyles();
  const { FirstName, LastName, Phone, checked } = person;
  return (
    <>
      <Box className={classes.listWrapper}>
        <Box className={classes.listInnerWrapper}>
          <Box>
            <FormControlLabel
              control={
                <Checkbox
                  checked={checked}
                  onChange={handleChange}
                  name={`${FirstName}_${LastName}`}
                  classes={{
                    checked: classes.checked,
                    root: classes.root
                  }}
                  checkedIcon={<CheckIcon className={classes.checkBoxIcon} />}
                  iconstyle={{ fill: 'black' }}
                />
              }
              classes={{
                root: classes.formControlLabelRoot
              }}
              label={`${FirstName} ${LastName}`}
            />
          </Box>

          <Box className={classes.phoneField}>
            <Typography>{Phone}</Typography>
            <Typography onClick={handleEdit} className={classes.editButton}>
              Edit
            </Typography>
          </Box>
        </Box>
      </Box>
      {touched && errors?.Phone && <ErrorBand error={errors?.Phone} />}
    </>
  );
}

export default AuthorizedPersonListRow;
