import React from 'react';
import RenderCheckbox from './RenderCheckbox';
import { FormControlLabel, Box } from '@material-ui/core';

import useStyles from './styles';

const LabeledCheckBox = ({ setFieldValue, dataTestId, disabled, label, value }) => {
  const classes = useStyles();
  const unCheckedDisabled = disabled;

  return (
    <Box className={classes.checkboxWrapper}>
      <FormControlLabel
        data-testid={`${dataTestId}-form`}
        disabled={disabled}
        control={
          <RenderCheckbox
            checked={value}
            handleCheck={setFieldValue}
            dataTestId={`${dataTestId}-checkbox`}
            unCheckedDisabled={unCheckedDisabled}
            disabled={disabled}
          />
        }
        label={label}
        classes={{
          root: classes.formControlLabelRoot,
          disabled: classes.formControlLabelDisabled
        }}
        onChange={setFieldValue}
      />
    </Box>
  );
};

export default LabeledCheckBox;
