import React from 'react';
import { FormControlLabel, Radio } from '@material-ui/core';
import cs from 'classnames';

import useStyles from './styles';

function RadioButton({ label, value, selectedValue, dataTestId }) {
  const classes = useStyles();
  return (
    <div className={classes.radioButtonWrapper}>
      <FormControlLabel
        control={
          <Radio
            classes={{
              root: classes.root
            }}
            checked={selectedValue === value}
            checkedIcon={<span className={cs(classes.icon, classes.checkedIcon)} />}
            inputProps={{
              'data-testId': dataTestId
            }}
          />
        }
        label={label}
        value={value}
        classes={{
          root: classes.formControlLabelRoot
        }}
        data-testId={`${dataTestId}-label`}
      />
    </div>
  );
}

export default RadioButton;
