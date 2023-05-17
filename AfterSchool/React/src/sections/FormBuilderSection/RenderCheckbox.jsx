import React from 'react';
import { makeStyles, Checkbox } from '@material-ui/core';
import { Check, RadioButtonUnchecked } from '@material-ui/icons';

function RenderCheckbox({ checked = false, handleCheck, dataTestId }) {
  const classes = makeStyles(() => ({
    checked: {
      color: '#494949 !important',
      padding: 0,
      borderRadius: 3,
      background: 'transparent !important'
    },
    root: {
      padding: 0,
      background: '#ffffff',
      color: '#ffffff',
      border: '1px solid #c4c4c4',
      borderRadius: 3,
      height: '18px',
      width: '18px',
      '& .MuiSvgIcon-root': {
        height: '18px',
        width: '18px'
      }
    }
  }))();

  return (
    <Checkbox
      data-testid={dataTestId}
      checked={checked}
      onChange={handleCheck}
      classes={{
        checked: classes.checked,
        root: classes.root
      }}
      icon={
        <RadioButtonUnchecked
          style={{
            height: '18px',
            width: '18px',
            borderRadius: 3
          }}
        />
      }
      checkedIcon={
        <Check
          style={{
            height: '18px',
            width: '18px',
            borderRadius: 3
          }}
        />
      }
      IconStyle={{ fill: 'black' }}
    />
  );
}

export default RenderCheckbox;
