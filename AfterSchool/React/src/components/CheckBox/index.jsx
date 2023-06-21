import React from 'react';
import { FormControlLabel, Checkbox, Box, FormHelperText } from '@material-ui/core';
import CheckIcon from '@material-ui/icons/Check';
import useStyles from './styles';

function CheckBox({ formik, question }) {
  const classes = useStyles();
  const { ID, Text: title, QuestionDataTypeId } = question;
  const { values, setFieldValue, errors, touched } = formik;

  const handleCheck = ({ target: { checked } }) => {
    setFieldValue(ID, checked ? 1 : 0);
  };

  return (
    <Box className={classes.outerWrapper}>
      <Box className={classes.innerWrapper}>
        <div className={classes.checkboxWrapper}>
          <FormControlLabel
            data-testid={`${title}-${QuestionDataTypeId}-form`}
            control={
              <Checkbox
                data-testid={`${title}-${QuestionDataTypeId}-checkbox`}
                checked={!!values[ID]}
                classes={{
                  checked: classes.checked,
                  root: classes.root
                }}
                checkedIcon={<CheckIcon className={classes.checkBoxIcon} />}
                iconstyle={{ fill: 'black' }}
              />
            }
            label={title}
            classes={{
              root: classes.formControlLabelRoot
            }}
            onChange={handleCheck}
          />
        </div>
        {touched[ID] && errors[ID] && (
          <Box className={classes.flexCenter}>
            <FormHelperText className={classes.maxWidth} error>
              {errors[ID]}
            </FormHelperText>
          </Box>
        )}
      </Box>
    </Box>
  );
}

export default CheckBox;
