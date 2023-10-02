import React from 'react';
import CheckIcon from '@material-ui/icons/Check';
import { Checkbox, FormControlLabel, Box, Typography } from '@material-ui/core';

import useStyles from './styles';
import { ErrorBand } from 'components';
import { withoutSignsPhoneNumber } from 'utils';

function AuthorizedPersonListRow({ handleChange, touched, person, errors, handleEdit }) {
  const classes = useStyles();
  const { ID, FirstName, LastName, Phone, HasAccess } = person;

  const canEdit = ID === -1;
  return (
    <>
      <Box className={classes.listWrapper}>
        <Box className={classes.listInnerWrapper}>
          <Box>
            <FormControlLabel
              control={
                <Checkbox
                  checked={HasAccess}
                  onChange={handleChange}
                  name={`${FirstName}_${LastName}`}
                  classes={{
                    checked: classes.checked,
                    root: classes.root
                  }}
                  checkedIcon={<CheckIcon className={classes.checkBoxIcon} />}
                  iconstyle={{ fill: 'black' }}
                  data-testid={`${FirstName}-${LastName}-checked`}
                />
              }
              classes={{
                root: classes.formControlLabelRoot
              }}
              label={`${FirstName} ${LastName}`}
            />
          </Box>

          <Box className={classes.phoneField}>
            <Typography>{withoutSignsPhoneNumber(Phone)}</Typography>
            {canEdit && (
              <Typography
                onClick={handleEdit}
                className={classes.editButton}
                data-testid="edit-button"
              >
                Edit
              </Typography>
            )}
          </Box>
        </Box>
      </Box>
      {touched && errors?.Phone && <ErrorBand error={errors?.Phone} />}
    </>
  );
}

export default AuthorizedPersonListRow;
