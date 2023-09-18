import React, { useEffect, useState } from 'react';
import { TextField as MuiTextField, Grid } from '@material-ui/core';

import useStyles from './styles';

const RatingQuestion = props => {
  const classes = useStyles();
  const { options, setOptions } = props;
  const { 0: min, length, [length - 1]: max } = options;

  const [values, setValues] = useState({
    minLabel: min?.value || '',
    minValue: Number(min?.key) || 0,
    maxLabel: max?.value || '',
    maxValue: Number(max?.key) || 5
  });

  useEffect(() => {
    const option = [];
    const { minLabel, minValue, maxLabel, maxValue } = values;
    option.push({ key: String(minValue), value: minLabel });
    for (let index = Number(minValue) + 1; index < maxValue; index++) {
      option.push({ key: String(index), value: '' });
    }
    option.push({ key: String(maxValue), value: maxLabel });
    setOptions(option);
  }, [values]);

  const changeHandler = (name, value) => {
    const isNumber = typeof value === 'number';
    if (isNumber) {
      const isMaxValue = name === 'maxValue';

      if (
        isMaxValue ? value <= values.minValue || value > 10 : value >= values.maxValue || value < 0
      ) {
        return;
      }
    }
    setValues({ ...values, [name]: value });
  };

  const TextField = fieldProps => {
    const { type } = fieldProps;

    return (
      <MuiTextField
        required
        fullWidth
        size="small"
        {...fieldProps}
        id="standard-basic"
        className={classes.formControlClass}
        onChange={e => {
          changeHandler(e.target.name, type === 'number' ? Number(e.target.value) : e.target.value);
        }}
      />
    );
  };

  return (
    <Grid container>
      <Grid item container lg={10} xs={10} sm={10} className={classes.marginAuto}>
        <Grid item lg={6} xs={12} sm={12} className={classes.displayFlex}>
          {TextField({
            type: 'number',
            key: 'minValue',
            name: 'minValue',
            value: values.minValue,
            'data-testid': 'min-value',
            placeholder: 'Minimum Value',
            InputProps: { min: 0, max: 9 }
          })}
        </Grid>
        <Grid item lg={6} xs={12} sm={12}>
          {TextField({
            key: 'minLabel',
            name: 'minLabel',
            value: values.minLabel,
            'data-testid': 'min-value-Label',
            placeholder: 'Label for Min Value (Optional)'
          })}
        </Grid>
      </Grid>
      <Grid item container lg={10} xs={10} sm={10} className={classes.marginAuto}>
        <Grid item lg={6} xs={12} sm={12} className={classes.displayFlex}>
          {TextField({
            type: 'number',
            key: 'maxValue',
            name: 'maxValue',
            value: values.maxValue,
            'data-testid': 'max-value',
            placeholder: 'Maximum Value',
            InputProps: { min: 1, max: 10 }
          })}
        </Grid>
        <Grid item lg={6} xs={12} sm={12}>
          {TextField({
            key: 'maxLabel',
            name: 'maxLabel',
            value: values.maxLabel,
            'data-testid': 'max-value-Label',
            placeholder: 'Label for Max Value (Optional)'
          })}
        </Grid>
      </Grid>
    </Grid>
  );
};

export default RatingQuestion;
