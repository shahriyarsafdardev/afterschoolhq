import React from 'react';
import RenderCheckbox from './RenderCheckbox';
import { FormControlLabel, makeStyles, Box } from '@material-ui/core';

const HideCheckBox = ({ question, setFieldValue, disabled, dataTestId }) => {
  const { IsActive: value } = question;
  const unCheckedDisabled = value && disabled;

  const classes = makeStyles(() => ({
    checkboxWrapper: {
      padding: '0px 6px'
    },
    formControlLabelRoot: {
      margin: '10px 0px',
      '& .MuiTypography-root': {
        marginLeft: 18
      }
    },
    formControlLabelDisabled: {
      opacity: '0.5',
      color: 'black !important'
    }
  }))();

  return (
    <Box className={classes.checkboxWrapper}>
      <FormControlLabel
        data-testid={`${dataTestId}-form`}
        disabled={disabled}
        control={
          <RenderCheckbox
            checked={!value}
            handleCheck={() => setFieldValue(!value)}
            dataTestId={`${dataTestId}-checkbox`}
            unCheckedDisabled={unCheckedDisabled}
            disabled={disabled}
          />
        }
        label={'Hide'}
        classes={{
          root: classes.formControlLabelRoot,
          disabled: classes.formControlLabelDisabled
        }}
        onChange={() => {
          setFieldValue(!value);
        }}
      />
    </Box>
  );
};

export default HideCheckBox;
