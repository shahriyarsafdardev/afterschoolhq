import React from 'react';
import { Link as LinkIcon, Check as CheckIcon } from '@material-ui/icons';
import { FormControlLabel, Checkbox, Box, FormHelperText } from '@material-ui/core';

import useStyles from './styles';

function CheckBox({ formik, question }) {
  const classes = useStyles();
  const { values, setFieldValue, errors, touched } = formik;
  const { ID, Text: title, QuestionDataTypeId, Link } = question;

  const handleCheck = ({ target: { checked } }) => {
    setFieldValue(ID, checked ? 1 : 0);
  };

  const LinkAndIcon = () => (
    <a className={classes.linkIcon} href={Link} target="_blank" rel="noreferrer">
      {'(Click here to view)'}
      <LinkIcon />
    </a>
  );

  const Label = () => (
    <Box className={classes.label}>
      {title} &nbsp; {Link && <LinkAndIcon />}
    </Box>
  );

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
            label={<Label />}
            classes={{
              root: classes.formControlLabelRoot
            }}
            onChange={handleCheck}
          />
        </div>
        {touched[ID] && errors[ID] && (
          <Box className={classes.flexCenter}>
            <FormHelperText
              data-testid={`${title}-${QuestionDataTypeId}-error`}
              className={classes.maxWidth}
              error
            >
              {errors[ID]}
            </FormHelperText>
          </Box>
        )}
      </Box>
    </Box>
  );
}

export default CheckBox;
